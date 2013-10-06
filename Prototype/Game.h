#pragma once
#include "PolycodeView.h"
#include "Polycode.h"

using namespace Polycode;

class GameState;

class Game : public EventHandler {
public:
	Core *core;

    Game(PolycodeView *view, int w, int h);
    virtual ~Game();
    
	virtual void init();
    virtual bool update();

	void changeWorld(GameState *world);
	GameState *getWorld();

	int getWidth() { return width; }
	int getHeight() { return height; }

protected:
	GameState *currentWorld;
	int width, height;

	virtual GameState *createFirstWorld();
	void setWorld(GameState *world);
};