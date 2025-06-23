#!/bin/bash

# 配置参数
REMOTE_USER="root"
REMOTE_IP="14.103.134.127"
PRIVATE_KEY="/Users/tanliyuan/.ssh/volcano.pem"
LOCAL_DIR="Builds/linux"
REMOTE_DIR="/root/server"

# 检查本地目录是否存在
if [ ! -d "$LOCAL_DIR" ]; then
  echo "错误：本地目录 $LOCAL_DIR 不存在！"
  exit 1
fi

# 使用rsync上传差异文件
echo "正在同步文件到远程服务器 (仅上传有差异的文件)..."
rsync -avz --checksum --progress -e "ssh -i $PRIVATE_KEY" \
      "$LOCAL_DIR/" "$REMOTE_USER@$REMOTE_IP:$REMOTE_DIR"

# 检查上传结果
if [ $? -eq 0 ]; then
  echo "✅ 同步成功！"
  echo "文件已同步到：$REMOTE_USER@$REMOTE_IP:$REMOTE_DIR"
else
  echo "❌ 同步失败，请检查错误信息"
  exit 1
fi