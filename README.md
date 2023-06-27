# pssh

Parallel SSH for running commands on multiple hosts simultaneously.

## Description

pssh is a C# program for executing a specified command on multiple SSH hosts in parallel. The command output and error messages are displayed on the console with a unique color for each host. The list of hosts, along with their configuration, is read from a JSON configuration file.

## Options

No command-line options are required. The command to execute on all hosts should be provided as command-line arguments.

## Environment Variables

* `NO_COLOR`: If this environment variable is set, all output will be printed in white, ignoring the host-specific color settings.
* `PSSH_CONFIG`: This environment variable can be set to the path of the configuration file. If not set, the configuration file at `~/.config/pssh/config.json` is used.

## Configuration File

The configuration file is a JSON file that specifies the list of hosts to connect to. Each host has the following properties:

* `address` (string, required): The IP address or hostname of the SSH server.
* `username` (string, optional): The username to use for the SSH connection. If not specified, the username of the current user is used.
* `port` (int, optional): The port to use for the SSH connection. If not specified, the default SSH port (22) is used.
* `key_file` (string, optional): The path to the private key file to use for the SSH connection.
* `color` (string, optional): The console color to use for this host's output. If not specified, a default color will be used.

The colors available are: Red, Green, Yellow, Blue, Magenta, Cyan, Gray, DarkRed, DarkGreen, DarkYellow, DarkBlue, DarkMagenta, and DarkCyan. 

Example configuration file:

```json
[
  {
    "address": "192.168.0.1",
    "username": "admin",
    "port": 22,
    "key_file": "/path/to/keyfile",
    "color": "Red"
  },
  {
    "address": "192.168.0.2",
    "username": "root",
    "port": 2222,
    "key_file": "/path/to/another/keyfile",
    "color": "Green"
  }
]
```

## Examples

Run `ls` on all configured hosts:

```
pssh ls
```

Run a command with spaces on all configured hosts:

```
pssh "ls -l /var/log"
```

Use a custom configuration file:

```
PSSH_CONFIG=/path/to/config.json pssh uname -a
```

Disable colors:

```
NO_COLOR=1 pssh uptime
```

## Exit Status

The `pssh` utility exits with one of the following values:

* `0` - Command was successfully executed on all hosts.
* `1` - An error occurred. Possible reasons include: no command provided, no hosts found in config file, or the config file does not exist.
