#pragma once
#include "GameEntity.h"
#include "PolyScreenSprite.h"

class Player : public GameEntity {
public:
	typedef enum Dir { LEFT, RIGHT };
	Dir dir;
	float speed;

	Player();
	~Player();

	virtual void Update();
	virtual void Render();

	virtual void init();

	void setDir(Dir dir);

protected:
	float baseSpeed;
	float maxSpeed;
	float baseAcceleration;
	float baseDeceleration;

	ScreenSprite *graphic;
};