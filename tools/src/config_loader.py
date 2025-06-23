import json
import sys
import os
import gen.cfg.schema as schema

sys.path.append(os.path.abspath('src/gen'))

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
    print("\nGet item:", item_id, end='')
def remove_item(item_id):
    items.remove(int(item_id))
    print("\nLost item:", item_id, end='')

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
        print(f"Unknown action: {action.f}")

def main():
    while True:

        if current is None or len(current) == 0:
            print("No dialogues available.")
            return
        
        while True:
            id = input(f"Please select: {','.join(map(str, current))}\n")
            try:
                id = int(id)
                if id in current:
                    break
            except:
                ...
            print("Invalid input, please try again.")

        index = 1
        while True:
            cfg = tables.TbDialogue.get(id, index)
            if cfg is None:
                print()
                break
            print(f"{cfg.group_id}-{cfg.id} {cfg.actor}: {cfg.text}", end='')
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
                    print(f"{i+1}. {dialogue.text}")

                while True:
                    try:
                        selection = int(input("Your selection: "))
                        dialogue = available_dialogues[selection-1]
                        id = dialogue.group_id
                        index = dialogue.id
                        break
                    except:
                        ...
                    print("Invalid input, please try again.")
            else:
                index += 1

if __name__ == "__main__":
    main() 