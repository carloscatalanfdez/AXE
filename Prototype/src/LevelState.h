#pragma once
#include "GameState.h"
#include "Polycode.h"

class LevelState : public GameState 
{
public:
	LevelState();
	~LevelState();

	void init();
	void Update();

	void handleEvent(Event *e);

protected:
	Polycode::ScreenImage *image;
};

