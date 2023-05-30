.PHONY: install build

BINARY_NAME=pssh
LOCAL_BIN_PATH=$(HOME)/.local/bin
CONFIG_PATH=$(HOME)/.config/pssh

build:
	@go build -o $(BINARY_NAME)

install: build
	@mkdir -p $(LOCAL_BIN_PATH)
	@mkdir -p $(CONFIG_PATH)
	@mv $(BINARY_NAME) $(LOCAL_BIN_PATH)/
	@cp config.json $(CONFIG_PATH)/
	@echo "Installed $(BINARY_NAME) to $(LOCAL_BIN_PATH) and config.json to $(CONFIG_PATH)"
