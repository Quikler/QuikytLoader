#!/bin/bash
# Simple script to navigate to publish directory.
# Usage: . output.sh <- Note: DO NOT RUN it like this './output.sh' because this runs the script in a new process, where any 'cd' affect only this subprocess.
# use "cd -" to go back to where you were before
source "$(dirname "${BASH_SOURCE[0]}")/config.sh"

cd "$(dirname "${BASH_SOURCE[0]}")/$STARTUP_PROJ_NAME/bin/Release/$TFM/$RID/publish/"
