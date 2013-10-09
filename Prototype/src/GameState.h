#pragma once
#include "PolyScreen.h"

using namespace Polycode;

class Game;
class GameEntity;

class GameState : public Screen  {
public:
	Game *game;

	GameState();
	virtual ~GameState();

	// Important: Init assumes the game attribute is already set in this class
	virtual void init();

	// Add entity is originally not virtual in Screen class,
	// so we have to redefine it
	virtual void addChild(GameEntity *entity);
	virtual void addChild(ScreenEntity *entity);
};

