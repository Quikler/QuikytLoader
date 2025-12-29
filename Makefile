.PHONY: aot trimmed clean

RID ?= linux-x64
PROJECT = QuikytLoader.Startup
PUBLISH_BASE_CMD = dotnet publish $(PROJECT) -r $(RID) -c Release

# IN OUR CASE
# aot: 53.9 MB
# trimmed: 25.1 MB

# TRIMMED ONLY (slower startup, less size, JIT at runtime)
trimmed:
	$(PUBLISH_BASE_CMD) -p:PublishTrimmed=true -p:TrimMode=full

# NATIVE AOT (bigger size, faster startup)
aot:
	$(PUBLISH_BASE_CMD) -p:PublishAot=true -p:OptimizationPreference=Size

clean:
	# Do not descend into bin/obj; delete them directly
	find . \( -name bin -o -name obj \) -type d -prune -exec rm -rf {} +
