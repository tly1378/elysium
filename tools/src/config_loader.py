import json
import sys
import os
import gen.cfg.schema as schema

sys.path.append(os.path.abspath('src/gen'))

# ANSI颜色代码
class Colors:
    RESET = "\033[0m"
    BOLD = "\033[1m"
    FAINT = "\033[2m"
    BLUE = "\033[94m"
    GREEN = "\033[92m"
    YELLOW = "\033[93m"
    RED = "\033[91m"

def loader(f):
    """JSON文件加载器"""
    return json.load(open('assets/data/' + f + ".json", 'r', encoding="utf-8"))

tables = schema.cfg_Tables(loader)
global_cfg = tables.TbGlobal.getData()
current = global_cfg.initial_dialogues
items = []
def add_group(group_id):
    current.append(int(group_id))
def remove_group(group_id):
    current.remove(int(group_id))
def add_item(item_id):
    items.append(int(item_id))
    print(f"\n{Colors.GREEN}Get item: {item_id}{Colors.RESET}", end='')
def remove_item(item_id):
    items.remove(int(item_id))
    print(f"\n{Colors.RED}Lost item: {item_id}{Colors.RESET}", end='')

actions = {
    "add_group": add_group,
    "remove_group": remove_group,
    "add_item":add_item,
    "remove_item":remove_item
}

def do_action(action):
    if action.f in actions:
        actions[action.f](action.p)
    else:
        print(f"{Colors.YELLOW}Unknown action: {action.f}{Colors.RESET}")

def main():
    while True:
        if current is None or len(current) == 0:
            print(f"{Colors.RED}No dialogues available.{Colors.RESET}")
            return
        
        while True:
            print(f"{Colors.BLUE}Current dialogues: {','.join(map(str, current))}{Colors.RESET}")
            id = input(">>> ")
            try:
                id = int(id)
                if id in current:
                    break
            except:
                ...

        index = 1
        while True:
            cfg = tables.TbDialogue.get(id, index)
            if cfg is None:
                print()
                break
            print(f"{Colors.BOLD}{cfg.group_id}-{cfg.id} {cfg.actor}: {Colors.RESET}{cfg.text}", end='')
            for action in cfg.actions:
                do_action(action)
            input()

            if cfg.selections != None and len(cfg.selections) > 0:
                available_dialogues = []
                for dialogue_id in cfg.selections:
                    dialogue = tables.TbDialogue.get(dialogue_id.group_id, dialogue_id.id)
                    if dialogue.item_requirements is None or all(item in items for item in dialogue.item_requirements):
                        available_dialogues.append(dialogue)
                for i, dialogue in enumerate(available_dialogues):
                    print(f"{Colors.YELLOW}{i+1}. {dialogue.text}{Colors.RESET}")

                while True:
                    selection_input = input(">>> ")
                    try:
                        selection = int(selection_input)
                        dialogue = available_dialogues[selection-1]
                        id = dialogue.group_id
                        index = dialogue.id
                        break
                    except:
                        ...
            else:
                index += 1

if __name__ == "__main__":
    main()