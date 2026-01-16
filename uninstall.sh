#!/bin/bash
# Export config.sh with common variables (better to export it before '-u' option or before variable use)
source "$(dirname "$0")/config.sh"

# '-e' exits on error to prevents scripts from silently continuing after a failure, '-u' exits on unset variables (protecting rm -rf /$APP_NAME if $APP_NAME is empty), and '-o pipefail' ensures errors in pipelines are caught.
set -euo pipefail

# Uninstall QuikytLoader
rm -f /usr/bin/$APP_NAME
rm -rf /opt/$APP_NAME/
