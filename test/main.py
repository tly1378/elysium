import networkx as nx
import matplotlib.pyplot as plt

# 设置 matplotlib 字体，确保中文显示正常
# 禁用字体的位图渲染
plt.rcParams['font.sans-serif'] = ['Heiti TC', 'Arial Unicode MS', 'SimHei', 'WenQuanYi Micro Hei']
plt.rcParams['axes.unicode_minus'] = False  # 解决负号显示问题

# 游戏地图数据
map_data = {
  "nodes": [
    {
      "id": "0",
      "name": "废弃超市",
      "description": "曾经繁华的大型超市，如今货架倾倒，商品腐烂，入口处被废弃的购物车堵住，散发着霉味和腐烂气味",
      "actions": ["搜索物资（可能找到罐头、药品）", "修复简易防御工事", "遭遇游荡丧尸"]
    },
    {
      "id": "1",
      "name": "医院废墟",
      "description": "部分建筑坍塌的医院，急诊室留有干涸的血迹，药房被洗劫过但可能有遗漏的药品，停尸间传来奇怪的声响",
      "actions": ["寻找药品和医疗设备", "检查病历档案（可能发现线索）", "躲避变异感染者"]
    },
    {
      "id": "2",
      "name": "幸存者营地",
      "description": "由废弃集装箱搭建的临时营地，有简单的围栏防御，住着少数幸存者，气氛紧张但有基本秩序",
      "actions": ["交易物资", "接受任务", "休息恢复体力", "获取情报"]
    },
    {
      "id": "3",
      "name": "地下避难所",
      "description": "政府建造的地下避难所，入口隐蔽且有厚重的金属门，内部可能有完好的通风系统和储备物资",
      "actions": ["探索未知区域", "启动备用电源", "遭遇封闭空间内的变异生物"]
    },
    {
      "id": "4",
      "name": "加油站",
      "description": "被遗弃的加油站，加油机早已空竭，便利店被烧毁一半，地下油罐可能还有剩余燃油",
      "actions": ["寻找燃油", "修复简易交通工具", "可能触发油罐爆炸陷阱"]
    },
    {
      "id": "5",
      "name": "倒塌的桥梁",
      "description": "横跨河流的桥梁中间部分坍塌，钢筋外露，下方水流浑浊，可能有变异水生生物",
      "actions": ["尝试修复桥梁", "寻找渡河方法", "防御来自两侧的敌人"]
    },
    {
      "id": "6",
      "name": "军事基地外围",
      "description": "被铁丝网和废弃坦克环绕的军事基地外围，有自动防御机枪（部分仍在运作），可能有武器库",
      "actions": ["破解防御系统", "寻找武器弹药", "躲避巡逻机器人"]
    },
    {
      "id": "7",
      "name": "居民区废墟",
      "description": "成片倒塌的居民楼，街道被汽车残骸堵塞，部分房屋可能有幸存者隐藏，也可能有丧尸巢",
      "actions": ["搜索民居（可能找到生活用品）", "解救被困人员", "清理丧尸巢"]
    }
  ],
  "edges": [
    {"from": "0", "to": "1"},
    {"from": "0", "to": "7"},
    {"from": "1", "to": "3"},
    {"from": "2", "to": "0"},
    {"from": "2", "to": "4"},
    {"from": "3", "to": "6"},
    {"from": "4", "to": "5"},
    {"from": "5", "to": "6"},
    {"from": "6", "to": "7"},
    {"from": "7", "to": "5"},
    {"from": "1", "to": "2"},
    {"from": "4", "to": "0"}
  ]
}

# 创建无向图
G = nx.Graph()

# 添加节点
node_labels = {}
node_descriptions = {}
for node in map_data["nodes"]:
    G.add_node(node["id"])
    node_labels[node["id"]] = node["name"]
    node_descriptions[node["id"]] = node["description"]

# 添加边
for edge in map_data["edges"]:
    G.add_edge(edge["from"], edge["to"])

# 设置布局
pos = nx.spring_layout(G, seed=42, k=0.3)  # seed确保布局一致，k控制节点间距

# 绘制图形
plt.figure(figsize=(14, 10))

# 绘制节点
nx.draw_networkx_nodes(G, pos, node_size=3000, node_color='darkred', alpha=0.8)

# 绘制边
nx.draw_networkx_edges(G, pos, width=2, edge_color='gray', alpha=0.6)

# 绘制节点标签
nx.draw_networkx_labels(G, pos, node_labels, font_size=12, font_weight='bold', 
                        verticalalignment='center')

# 添加标题
plt.title("末世游戏地图", fontsize=18, fontweight='bold')

# 隐藏坐标轴
plt.axis('off')

# 调整布局
plt.tight_layout()

# 显示图形
plt.show()

# 打印节点详细信息
print("\n节点详细信息:")
for node in map_data["nodes"]:
    print(f"\n{node['name']} (ID: {node['id']})")
    print(f"描述: {node['description']}")
    print("可执行操作:")
    for action in node["actions"]:
        print(f"- {action}")
