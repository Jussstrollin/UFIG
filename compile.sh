#!/bin/bash

# Compile
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishAot=true

# Check if compilation succeeded
if [ $? -eq 0 ]; then
        # Success: run the game
        ./bin/Release/net10.0/linux-x64/publish/Gaem
        else
                # Failure: print error and exit
                echo "Compilation failed. Not running old binary."
                exit 1
                fi
