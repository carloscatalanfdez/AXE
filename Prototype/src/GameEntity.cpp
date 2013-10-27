#include "GameEntity.h"


GameEntity::GameEntity() {
	isPersistent = false;
}

GameEntity::~GameEntity() {
	// GameState clears all his children
	for (int i = 0; i < getNumChildren(); i++) {
		Entity *entity = getChildAtIndex(i);
		delete entity;
	}
}
