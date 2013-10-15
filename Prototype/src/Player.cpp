#include "Player.h"
#include "Game.h"
#include "LeveLState.h"

// Input is a bitmask
#define LEFT_BIT 1
#define RIGHT_BIT 1 << 1
#define UP_BIT 1 << 2
#define DOWN_BIT 1 << 3

#define LEFT_MASK LEFT_BIT
#define RIGHT_MASK RIGHT_BIT
#define UP_MASK UP_BIT
#define DOWN_MASK DOWN_BIT

Player::Player() {
	isPersistent = true;

	// Constants
	baseSpeed = 3;
	maxSpeed = 10;
	baseAcceleration = 0.5;
	baseDeceleration = 2;

	// Initial values
	speed = 0;
	dir = Player::RIGHT;

	graphic = new ScreenImage("Assets/knight.png");
	addChild(graphic);
}

Player::~Player() {
	// Parent deletes children hierarchy
}

void Player::Update() {
	// Gather input
	short inputValue = 0;
	Polycode::CoreInput *input = game->core->getInput();
	if (input->getKeyState(Polycode::KEY_LEFT)) {
		inputValue |= LEFT_MASK;
	} else if (input->getKeyState(Polycode::KEY_RIGHT)) {
		inputValue |= RIGHT_MASK;
	}

	// Apply movement input
	if (inputValue & LEFT_MASK) {
		if (speed > -baseSpeed) {
			speed = -baseSpeed;
		} else if (speed > -maxSpeed) {
			speed -= baseAcceleration;
		}
	} else if (inputValue & RIGHT_MASK) {
		if (speed < baseSpeed) {
			speed = baseSpeed;
		} else if (speed < maxSpeed) {
			speed += baseAcceleration;
		}
	} else {
		if (speed > 0 && speed > baseDeceleration) {
			speed -= baseDeceleration;
		} else if (speed < 0 && speed < -baseDeceleration) {
			speed += baseDeceleration;
		} else {
			speed = 0;
		}
	}

	Vector2 pos = getPosition2D();
	setPosition(pos.x + speed, pos.y);

	// Adjust entity direction and position depending on were he's looking at
	if (speed > 0) {
		setDir(RIGHT);
	} else if (speed < 0) {
		setDir(LEFT);
	}

	GameEntity::Update();

	if (input->getKeyState(Polycode::KEY_f)) {
		LevelState *level = dynamic_cast<LevelState*>(world);
		if (level) {
			level->completeLevel();
		}
	}
}

void Player::Render() {
	GameEntity::Render();
}

void Player::setDir(Dir dir) {
	if (this->dir != dir) {
		this->dir = dir;

		// Hack: scale by -1 to mirror to the left (origin is now top right, we need to translate the entity)
		int inc = dir == RIGHT ? 1 : -1;
		setScaleX(inc);
		position.x -= inc*graphic->width;

	}
}