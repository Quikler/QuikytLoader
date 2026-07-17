.PHONY: all aot trimmed clean

# Default target
all: trimmed

TFM ?= $(shell . ./scripts/config.sh && echo $$TFM)
RID ?= $(shell . ./scripts/config.sh && echo $$RID)
STARTUP_PROJ_NAME := $(shell . ./scripts/config.sh && echo $$STARTUP_PROJ_NAME)
PUBLISH_BASE_CMD = dotnet publish $(STARTUP_PROJ_NAME) -r $(RID) -c Release

# IN OUR CASE
# aot: 53.9 MB
# trimmed: 25.1 MB

# TRIMMED ONLY (slower startup, less size, JIT at runtime)
trimmed:
	$(PUBLISH_BASE_CMD) -p:PublishTrimmed=true -p:TrimMode=full
	@echo "Trimmed app published at: ./$(STARTUP_PROJ_NAME)/bin/Release/$(TFM)/$(RID)/publish"

# NATIVE AOT (bigger size, faster startup)
aot:
	$(PUBLISH_BASE_CMD) -p:PublishAot=true -p:OptimizationPreference=Size
	@echo "AOT app published at: ./$(STARTUP_PROJ_NAME)/bin/Release/$(TFM)/$(RID)/publish"

clean:
	@# Do not descend into bin/obj; delete them directly
	find . \( -name bin -o -name obj \) -type d -prune -exec rm -rf {} +

run:
	dotnet run --project $(STARTUP_PROJ_NAME)
