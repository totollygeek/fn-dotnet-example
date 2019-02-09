# Fn .NET Example

The purpose of this project is to demonstrate how you can run .NET code inside Fn serverless function.

# Disclaimer

This is a VERY rough example. Its purpose is to show conceptually how you can run C# code, which is not an officially supported language in Fn, with their serverless framework. This is FAR from production ready.

## What is it?
- What is Fn you can learn from [here](https://github.com/fnproject/fn)
- The sources used for the example are [here](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SpamDetection)
- Docker is [here](https://www.docker.com)

## What you need installed
- Windows 10 WITH [Linux subsystem](https://docs.microsoft.com/en-us/windows/wsl/install-win10)
- [Docker](https://www.docker.com/get-started)
- [Fn CLI](https://github.com/fnproject/fn)
- [.NET Core 2.2](https://dotnet.microsoft.com/download)

## Get things up and running

### 1) Clone this repo

`git clone https://github.com/totollygeek/fn-dotnet-example.git`

### 2) Run `createtar.ps1` script

This will crate an init.tar file used for the init image

### 3) Run Fn with `fn start -d`

### 4) Run the `deploy.sh` script under Linux subsystem

Running the Fn server under Windows instead of the Linux subsystem still has some issues

### 5) Invoke the function

You can do it with `echo "some text to test" | fn invoke dontet spam`

This should invoke the function and show you the output in the console.

If you have suggestions or questions, don't hesitate to contact me here in GitHub or in the [Fn Slack](http://slack.fnproject.io/)