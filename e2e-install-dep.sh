#!/bin/bash

apt update -q -y
apt --yes install libnss3 xvfb libxi6 libgconf-2-4 unzip gnupg wget
wget -nv https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb
apt install ./google-chrome-stable_current_amd64.deb -y
uname -a
ls /usr/bin | grep -i google
google-chrome --version