package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"flag"
	"fmt"
	"io/ioutil"
	"os"
	"os/exec"
	"os/user"
	"path"
	"strings"
	"sync"
)

var defaultColors = []string{
	"\033[0;31m",
	"\033[0;32m",
	"\033[0;33m",
	"\033[0;34m",
	"\033[0;35m",
	"\033[0;36m",
	"\033[0;37m",
	"\033[1;31m",
	"\033[1;32m",
	"\033[1;33m",
	"\033[1;34m",
	"\033[1;35m",
	"\033[1;36m",
}

var noColor = "\033[0m"

type Host struct {
	Address  string `json:"address"`
	Color    string `json:"color"`
	Username string `json:"username"`
	Port     int    `json:"port"`
	KeyFile  string `json:"key_file"`
}

func printUsage() {
	fmt.Println("Usage: go run main.go [--help] [--config config.json] 'command to execute'")
	os.Exit(1)
}

func parseFlags() bool {
	help := flag.Bool("help", false, "Display Help")

	flag.Parse()

	return *help
}

func getCommand() string {
	if len(os.Args) < 2 {
		printUsage()
	}

	if os.Args[1] == "--" {
		if len(os.Args) < 3 {
			fmt.Println("Please provide the command to execute as an argument after '--'.")
			printUsage()
		}
		return strings.Join(os.Args[2:], " ")
	} else {
		if len(os.Args) > 2 {
			fmt.Println("Please provide the command to execute as a single argument, or use '--' for multiple arguments.")
			printUsage()
		}
		return os.Args[1]
	}
}

func loadConfig() []Host {
	configFilePath := os.Getenv("PSSH_CONFIG")
	if configFilePath == "" {
		configFilePath = path.Join(os.Getenv("HOME"), ".config", "pssh", "config.json")
	}

	configFile, err := os.Open(configFilePath)
	if err != nil {
		fmt.Printf("Error opening config file: %v\n", err)
		os.Exit(1)
	}
	defer configFile.Close()

	byteValue, _ := ioutil.ReadAll(configFile)

	var hosts []Host
	err = json.Unmarshal(byteValue, &hosts)
	if err != nil {
		fmt.Printf("Error parsing config file: %v\n", err)
		os.Exit(1)
	}

	return hosts
}

func getMaxAddressLength(hosts []Host) int {
	maxLength := 0
	for _, host := range hosts {
		if len(host.Address) > maxLength {
			maxLength = len(host.Address)
		}
	}
	return maxLength
}

func executeCommand(host Host, color string, maxLength int, command string) {
	sshCommand := []string{"ssh"}

	if host.Username != "" {
		sshCommand = append(sshCommand, fmt.Sprintf("%s@%s", host.Username, host.Address))
	} else {
		currentUser, err := user.Current()
		if err != nil {
			fmt.Printf("Error: %v\n", err)
			os.Exit(1)
		}
		sshCommand = append(sshCommand, fmt.Sprintf("%s@%s", currentUser.Username, host.Address))
	}

	if host.Port != 0 {
		sshCommand = append(sshCommand, "-p", fmt.Sprintf("%d", host.Port))
	}

	if host.KeyFile != "" {
		sshCommand = append(sshCommand, "-i", host.KeyFile)
	}

	sshCommand = append(sshCommand, command)

	cmd := exec.Command(sshCommand[0], sshCommand[1:]...)

	var stdout, stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr
	err := cmd.Run()

	if err != nil {
		fmt.Printf("%-*s Error executing command: %v\n", maxLength, host.Address, err)
		if stderr.Len() > 0 {
			fmt.Printf("%-*s Error details: %s\n", maxLength, host.Address, stderr.String())
		}
		return
	}

	scanner := bufio.NewScanner(&stdout)
	for scanner.Scan() {
		line := scanner.Text()

		if color != "" {
			fmt.Printf("%s%-*s%s %s\n", color, maxLength, host.Address, noColor, line)
		} else {
			fmt.Printf("%-*s %s\n", maxLength, host.Address, line)
		}
	}
}

func main() {
	help := parseFlags()

	if help {
		printUsage()
	}

	command := getCommand()

	if os.Getenv("NO_COLOR") != "" {
		defaultColors = []string{""}
		noColor = ""
	}

	hosts := loadConfig()
	maxLength := getMaxAddressLength(hosts)

	colorIndex := 0
	var wg sync.WaitGroup

	for _, host := range hosts {
		color := host.Color
		if color == "" {
			color = defaultColors[colorIndex%len(defaultColors)]
			colorIndex++
		}

		wg.Add(1)

		go func(host Host, color string) {
			defer wg.Done()

			executeCommand(host, color, maxLength, command)
		}(host, color)
	}

	wg.Wait()
}
