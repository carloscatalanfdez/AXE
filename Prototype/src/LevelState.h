#pragma once
#include "GameState.h"
#include "Polycode.h"

class Player;

class LevelState : public GameState  {
public:
	union {
		// Variable access to first and second player
		struct { Player *playerOne, *playerTwo; };
		// Array access to all players
		Player *players[2];
	};
	// Number of active players
	short numPlayers;

	LevelState();
	~LevelState();

	void init();
	void Update();

	// Complete level and move to the next level
	void completeLevel();
	// Complete level and move the the specified level
	void completeLevel(int nextLevel);

	// Load level with given id
	void loadLevel(int levelId);
	// Load level reusing given players
	void loadLevel(int levelId, Player* currentPlayers[]);

protected:
	// Identifier of the current level
	int id;
};

