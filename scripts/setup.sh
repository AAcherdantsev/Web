#!/usr/bin/env bash
set -e

# Resolve paths relative to this script, so it works from any working directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION="$SCRIPT_DIR/../Web.slnx"

echo "=== Setting up environment for Aspire project ==="
echo

# --- Check and install .NET SDK ---
if command -v dotnet &>/dev/null; then
    echo ".NET SDK found: $(dotnet --version)"
else
    echo ".NET SDK not found. Installing .NET 10..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
    bash dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
    rm dotnet-install.sh

    # Add .NET to PATH if needed
    if [[ ":$PATH:" != *":$HOME/.dotnet:"* ]]; then
        echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
        echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.zshrc 2>/dev/null || true
    fi

    export PATH="$HOME/.dotnet:$PATH"
    echo ".NET SDK installed successfully: $(dotnet --version)"
fi

# --- Check and install Docker ---
if command -v docker &>/dev/null; then
    echo "Docker found: $(docker --version)"
else
    echo "Docker not found. Installing Docker..."

    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        curl -fsSL https://get.docker.com -o get-docker.sh
        sudo sh get-docker.sh
        rm get-docker.sh

        # Add current user to docker group
        if groups $USER | grep &>/dev/null '\bdocker\b'; then
            echo "User already in 'docker' group."
        else
            sudo usermod -aG docker $USER
            echo "Added user to 'docker' group. Please log out or run 'newgrp docker'."
        fi

    elif [[ "$OSTYPE" == "darwin"* ]]; then
        echo "macOS detected."
        arch_name="$(uname -m)"

        if [[ "$arch_name" == "arm64" ]]; then
            echo "Apple Silicon detected."
            docker_url="https://desktop.docker.com/mac/stable/arm64/Docker.dmg"
        else
            echo "Intel Mac detected."
            docker_url="https://desktop.docker.com/mac/stable/amd64/Docker.dmg"
        fi

        echo "Downloading Docker Desktop..."
        curl -L -o ~/Downloads/Docker.dmg "$docker_url"
        echo "Download complete."
        echo "Open ~/Downloads/Docker.dmg and drag Docker to Applications to finish installation."
    else
        echo "Unsupported OS: $OSTYPE. Please install Docker manually."
    fi
fi

# --- Restore NuGet packages ---
if [ -f "$SOLUTION" ]; then
    echo "Restoring NuGet packages..."
    dotnet restore "$SOLUTION"
else
    echo "No .slnx file found. Skipping 'dotnet restore'."
fi

echo
echo "=== Setup complete! ==="
echo "If Docker was installed for the first time, please restart your system."
