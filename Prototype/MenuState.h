#pragma once
#include "GameState.h"
#include "Polycode.h"

class Player;

class MenuState : public GameState
{
public:
	MenuState();
	~MenuState();

	void init();
	void Update();

	void handleEvent(Event *e);

protected:
	ScreenLabel *label;
};

