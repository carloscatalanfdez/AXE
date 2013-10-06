#pragma once
#include "GameState.h"
#include "Polycode.h"

class MenuState : public GameState
{
public:
	MenuState();
	~MenuState();

	void init();

protected:
	ScreenLabel *label;
};

