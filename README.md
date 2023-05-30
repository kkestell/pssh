# Parallel SSH (`pssh`)

`pssh` is a Go application that allows you to run commands on multiple servers in parallel. It allows for configuration of hosts, usernames, ports, key files, and optional terminal colors for output differentiation.

## Installation

You can install the program with the provided Makefile. The `install` target builds the program and copies the executable to `~/.local/bin` and the configuration file to `~/.config/pssh/config.json`. Here are the steps:

1. Clone the repository: `git clone https://github.com/username/pssh.git`
2. Change into the project directory: `cd pssh`
3. Install the application: `make install`

Note: Make sure `~/.local/bin` is in your `$PATH` for easy usage.

## Configuration

The program reads a configuration file in JSON format located at `~/.config/pssh/config.json`. This file holds information about the servers you want to run commands on.

Here is an example configuration file:

```json
[
  {
    "address": "10.0.0.2",
    "username": "root",
    "port": "22",
    "key_file": "~/.ssh/id_rsa",
    "color": "\033[0;31m"
  },
  {
    "address": "10.0.0.3",
    "username": "user",
    "port": "2222",
    "key_file": "~/.ssh/id_rsa"
  }
]
```

## Usage

To run a command on the servers defined in your configuration file, use the command:

```bash
pssh 'your-command-here'
```

You can also pass multiple arguments with the `--` option:

```bash
pssh -- your command with multiple arguments
```

For example, to list the contents of the `/var/www` directory, you would use:

```bash
pssh -- ls /var/www
```

## Troubleshooting

If you encounter any issues with running commands that require `sudo`, remember that the `sudo` command may require interactive password input, which is not supported in this program. Consider configuring passwordless `sudo` on your servers for commands that need it.
