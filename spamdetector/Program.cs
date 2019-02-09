using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

///////////////////////////////
// ML code example taken from:
// https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SpamDetection
///////////////////////////////

namespace fn
{
	class Program
	{
		private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
		private static string TrainDataPath => Path.Combine(AppPath, "spamdata");

		static void Main(string[] args)
		{
			var stdInput = Console.In.ReadToEnd();

			if (string.IsNullOrEmpty(stdInput))
			{
				Console.WriteLine("No input");
				return;
			}

			try
			{
				var predictor = GetPredictor();
				var input = new SpamInput { Message = stdInput.TrimEnd(Environment.NewLine.ToArray()) };
				var prediction = predictor.Predict(input);

				Console.WriteLine("The message '{0}' is {1}", input.Message, prediction.isSpam ? "SPAM" : "NOT spam");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return;
			}
		}

		private static PredictionFunction<SpamInput, SpamPrediction> _predictor;
		private static PredictionFunction<SpamInput, SpamPrediction> GetPredictor()
		{
			if (_predictor == null)
			{
				// Set up the MLContext, which is a catalog of components in ML.NET.
				var mlContext = new MLContext();

				// Create the reader and define which columns from the file should be read.
				var reader = new TextLoader(mlContext, new TextLoader.Arguments()
				{
					Separator = "tab",
					HasHeader = true,
					Column = new[]
						{
						new TextLoader.Column("Label", DataKind.Text, 0),
						new TextLoader.Column("Message", DataKind.Text, 1)
					}
				});

				var data = reader.Read(new MultiFileSource(TrainDataPath));

				// Create the estimator which converts the text label to boolean, featurizes the text, and adds a linear trainer.
				var estimator = mlContext.Transforms.CustomMapping<MyInput, MyOutput>(MyLambda.MyAction, "MyLambda")
					.Append(mlContext.Transforms.Text.FeaturizeText("Message", "Features"))
					.Append(mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent());

				// Evaluate the model using cross-validation.
				// Cross-validation splits our dataset into 'folds', trains a model on some folds and 
				// evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
				// Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
				var cvResults = mlContext.BinaryClassification.CrossValidate(data, estimator, numFolds: 5);
				var aucs = cvResults.Select(r => r.metrics.Auc);

				// Now let's train a model on the full dataset to help us get better results
				var model = estimator.Fit(data);

				// The dataset we have is skewed, as there are many more non-spam messages than spam messages.
				// While our model is relatively good at detecting the difference, this skewness leads it to always
				// say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
				// it is useful to look at the precision-recall curve to identify the best possible threshold.
				var inPipe = new TransformerChain<ITransformer>(model.Take(model.Count() - 1).ToArray());
				var lastTransformer = new BinaryPredictionTransformer<IPredictorProducing<float>>(mlContext, model.LastTransformer.Model, inPipe.GetOutputSchema(data.Schema), model.LastTransformer.FeatureColumn, threshold: 0.15f, thresholdColumn: DefaultColumnNames.Probability);

				ITransformer[] parts = model.ToArray();
				parts[parts.Length - 1] = lastTransformer;
				var newModel = new TransformerChain<ITransformer>(parts);

				// Create a PredictionFunction from our model 
				_predictor = newModel.MakePredictionFunction<SpamInput, SpamPrediction>(mlContext);
			}

			return _predictor;
		}
	}

	class MyInput
	{
		public string Label { get; set; }
	}

	class MyOutput
	{
		public bool Label { get; set; }
	}

	class MyLambda
	{
		[Export("MyLambda")]
		public ITransformer MyTransformer => ML.Transforms.CustomMappingTransformer<MyInput, MyOutput>(MyAction, "MyLambda");

		[Import]
		public MLContext ML { get; set; }

		public static void MyAction(MyInput input, MyOutput output)
		{
			output.Label = input.Label == "spam" ? true : false;
		}
	}

	class SpamInput
	{
		public string Label { get; set; }
		public string Message { get; set; }
	}

	class SpamPrediction
	{
		[ColumnName("PredictedLabel")]
		public bool isSpam { get; set; }

		public float Score { get; set; }
		public float Probability { get; set; }
	}
}
