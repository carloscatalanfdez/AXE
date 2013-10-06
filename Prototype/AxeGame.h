#pragma once
#include "Game.h"

class AxeGame : public Game {
public:
	AxeGame(PolycodeView* view);
	~AxeGame();

protected:
	GameState *initFirstWorld();
};

