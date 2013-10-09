#pragma once
#include "PolyScreenEntity.h"

using namespace Polycode;

class GameState;
class Game;

class GameEntity : public ScreenEntity {
public:
	// Establishes whether the entity should be deleted along with the GameState or not
	bool isPersistent;
	GameState *world;
	Game *game;

	GameEntity();
	virtual ~GameEntity();

	virtual void init() {}
};

