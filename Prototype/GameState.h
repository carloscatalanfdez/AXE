#pragma once
#include "PolyScreen.h"

using namespace Polycode;

class Game;

class GameState : public Screen {
public:
	Game *game;

	GameState();
	virtual ~GameState();

	virtual void init() {}
};

