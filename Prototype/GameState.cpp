#include "GameState.h"
#include "GameEntity.h"

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