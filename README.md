# CoffeeRun

This repository contains the prototype scaffolding for CoffeeRun (Coffee Van Tycoon).

Included in this branch:

- Assets/Scripts/*.cs : ScriptableObject definitions and prototype managers
- Assets/Editor/JSONToScriptableObjectsEditor.cs : Editor tool to import JSON into ScriptableObjects
- Assets/Resources/Data/*.json : ingredients, recipes, cards (sample data)
- backlog.csv : Trello/Jira importable backlog

Quick start (Unity):
1. Open this project in Unity (2020.3+ recommended). The scripts will compile and Unity will generate .meta files.
2. Run Tools -> CoffeeVan -> Import Game Data to create ScriptableObjects from the JSON files.
3. Implement GameManager and hook up scenes. See README for suggested scene layout.

