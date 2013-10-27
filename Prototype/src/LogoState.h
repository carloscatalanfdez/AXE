#pragma once
#include "GameState.h"
#include "PolyScreenImage.h"

class LogoState : public GameState {
public:
	LogoState();
	virtual ~LogoState();

	void init();
	void Update();

protected:
	ScreenImage *logo;
};