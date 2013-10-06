#pragma once
#include "PolyScreenEntity.h"

using namespace Polycode;

class GameEntity : public ScreenEntity {
public:
	// Establishes whether the entity should be deleted along with the GameState or not
	bool isPersistent;

	GameEntity();
	~GameEntity();
};

