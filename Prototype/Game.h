#pragma once
#include "PolycodeView.h"
#include "Polycode.h"

using namespace Polycode;

class GameState;

class Game : public EventHandler {
public:
    Game(PolycodeView *view, int w, int h);
    virtual ~Game();
    
    virtual bool update();

	void changeWorld(GameState *world);
	GameState *getWorld();

protected:
	Core *core;
	GameState *currentWorld;
	int width, height;

	virtual GameState *initFirstWorld();
};