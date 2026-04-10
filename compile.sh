#!/bin/bash

# Compile
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishAot=true

# Check if compilation succeeded
if [ $? -eq 0 ]; then
        # Success: run the game
        ./bin/Release/net6.0/linux-x64/Gaem
        else
                # Failure: print error and exit
                echo "Compilation failed. Not running old binary."
                exit 1
                fi
