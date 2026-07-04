#!/bin/bash

# Define your output file
OUTPUT_FILE="all_files.txt"

# Clear the output file if it already exists (optional)
 > "$OUTPUT_FILE"

# Iterate through all files in the current directory
for file in *; do
	# Check if it is a regular file and not the output file itself or the script
	if [ -f "$file" ] && [ "$file" != "$OUTPUT_FILE" ] && [ "$file" != "collect_files.sh" ]; then
		echo "--- ($file) ---" >>"$OUTPUT_FILE"
		echo "" >>"$OUTPUT_FILE"
		cat "$file" >>"$OUTPUT_FILE"
		echo -e "\n\n-------------------\n" >>"$OUTPUT_FILE"
	fi
done

echo "Done! All files appended to $OUTPUT_FILE."
