#!/bin/bash

# Compile
dotnet build -c Release

# Check if compilation succeeded
if [ $? -eq 0 ]; then
	# Success: run the game
	./bin/Release/net8.0/Gaem
else
	# Failure: print error and exit
	echo "Compilation failed. Not running old binary."
	exit 1
fi
