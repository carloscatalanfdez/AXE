#pragma once
#include "Game.h"

class Player;

class AxeGame : public Game {
public:
	AxeGame(PolycodeView* view);
	~AxeGame();

	void init();
	bool update();
	
	void launchMenu();
	void launchFirstLevel();

protected:
	Player *players[2];

	GameState *createFirstWorld();
};

