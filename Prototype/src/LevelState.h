#pragma once
#include "GameState.h"
#include "Polycode.h"

class Player;

class LevelState : public GameState 
{
public:
	LevelState();
	~LevelState();

	void init();
	void Update();

protected:
	Player *player;
};

