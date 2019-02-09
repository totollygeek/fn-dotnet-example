#!/bin/sh
docker build -t fn-dotnet -f Dockerfile-init-image .
fn --verbose init --init-image=fn-dotnet spam
cd spam
fn --verbose deploy --app dotnet --local