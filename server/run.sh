#!/bin/bash

# 配置参数
REMOTE_USER="root"
REMOTE_IP="14.103.134.127"
PRIVATE_KEY="/Users/tanliyuan/.ssh/volcano.pem"
REMOTE_DIR="/root/server"
APP_NAME="Elysium"
PORT="80"

# 修复私钥权限问题
chmod 600 "$PRIVATE_KEY"

# 通过SSH在服务器上运行应用程序
echo "在服务器上启动应用程序..."
ssh -i "$PRIVATE_KEY" "$REMOTE_USER@$REMOTE_IP" << EOF
  # 进入应用目录
  cd $REMOTE_DIR
  
  # 停止之前可能运行的实例
  pkill -f $APP_NAME || true
  
  # 启动应用程序并监听8080端口
  nohup ./$APP_NAME --urls "http://*:$PORT" > app.log 2>&1 &
  
  # 检查是否启动成功
  sleep 2
  if pgrep -f $APP_NAME > /dev/null; then
    echo "✅ 应用程序已在端口 $PORT 上成功启动"
    echo "日志文件: $REMOTE_DIR/app.log"
  else
    echo "❌ 应用程序启动失败，请检查日志"
    exit 1
  fi
EOF

echo "操作完成。"