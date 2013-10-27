#include "GameState.h"
#include "GameEntity.h"

#include <Polycode.h>

GameState::GameState() {
	// Disabled by default until it's activated inside a Game
	// This way we prevent it from entering the rendering pipeline
	enabled = false;
	game = NULL;
}

GameState::~GameState() {
	// GameState clears all screen non-persistent entities
	for (int i = 0; i < rootEntity.getNumChildren(); i++) {
		Entity *entity = rootEntity.getChildAtIndex(i);
		GameEntity *gameEntity = dynamic_cast<GameEntity*>(entity);
		if (!(gameEntity && gameEntity->isPersistent)) {
			delete entity;
		}
	}
}

void GameState::init() {
	// Fill in world entities' parameters
	for (int i = 0; i < rootEntity.getNumChildren(); i++) {
		Entity *entity = rootEntity.getChildAtIndex(i);
		GameEntity *gameEntity = dynamic_cast<GameEntity*>(entity);
		if (gameEntity) {
			gameEntity->game = game;
			gameEntity->world = this;
			gameEntity->init();
		}
	}
}

void GameState::addChild(GameEntity *entity) {
	// Fill entity's parameters
	entity->world = this;
	if (game) {
		entity->game = game;
		entity->init();
	}

	Screen::addChild(entity);
}

void GameState::addChild(ScreenEntity *entity) {
	Screen::addChild(entity);
}