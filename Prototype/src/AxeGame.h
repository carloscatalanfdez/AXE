#pragma once
#include "Game.h"

class AxeGame : public Game {
public:
	AxeGame(PolycodeView* view);
	~AxeGame();

	void init();
	bool update();

protected:
	GameState *createFirstWorld();
};

