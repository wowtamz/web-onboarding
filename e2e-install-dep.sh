#!/bin/bash

apt-get update && apt-get install -y wget
wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get update && apt-get install -y apt-transport-https
apt-get update && apt-get install -y dotnet-sdk-6.0
apt-get install -y sudo
sudo apt-get update
sudo apt-get install -y openjdk-11-jre-headless
sudo apt-get install -y xvfb
sudo apt-get install -y libxi6 libgconf-2-4
apt-get update && apt-get install -y gnupg
wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add -
echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list
apt-get update
apt-get install -y google-chrome-stable
apt-get install -y unzip
wget https://chromedriver.storage.googleapis.com/92.0.4515.107/chromedriver_linux64.zip
unzip chromedriver_linux64.zip
mv chromedriver /usr/bin/chromedriver
chmod +x /usr/bin/chromedriver
export DISPLAY=:99
Xvfb :99 -screen 0 1920x1080x24 > /dev/null 2>&1 &
sleep 3