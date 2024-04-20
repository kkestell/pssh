.PHONY: all build install uninstall clean help

PREFIX := ~/.local
BINDIR := $(PREFIX)/bin

all: build

build:
	cd Pssh && dotnet build

install: build
	cd Pssh && \
	dotnet publish -r linux-x64 -c Release --self-contained true -o publish && \
	mkdir -p $(BINDIR) && \
	cp publish/pssh $(BINDIR)/

uninstall:
	rm -f $(BINDIR)/pssh

clean:
	cd Pssh && \
	rm -rf bin obj publish

help:
	@echo "Usage: make [target]"
	@echo "Available targets:"
	@echo "  all       - Build the project (default target)"
	@echo "  build     - Build the project"
	@echo "  install   - Install the project to BINDIR under PREFIX"
	@echo "  uninstall - Remove installed files from BINDIR"
	@echo "  clean     - Clean up build artifacts"
	@echo "  help      - Display this help"
	@echo "Customization Variables:"
	@echo "  PREFIX    - Set the installation root directory (default: ~/.local)"
	@echo "              Example: make install PREFIX=/usr/local"
	@echo "  BINDIR    - Set the binary installation directory (default: $(PREFIX)/bin)"
	@echo "              Example: make install BINDIR=/usr/bin"
	@echo "Note: BINDIR is relative to PREFIX unless an absolute path is given."

