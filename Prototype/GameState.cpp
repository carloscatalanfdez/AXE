#include "GameState.h"

GameState::GameState() {
	// Disabled by default until it's activated inside a Game
	// This way we prevent it from entering the rendering pipeline
	enabled = false;
	game = NULL;
}

GameState::~GameState() {
}
