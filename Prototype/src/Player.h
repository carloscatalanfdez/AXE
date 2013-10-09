#pragma once
#include "GameEntity.h"
#include "PolyScreenImage.h"

class Player : public GameEntity {
public:
	typedef enum Dir { LEFT, RIGHT };
	Dir dir;
	float speed;

	Player();
	~Player();

	virtual void Update();
	virtual void Render();

	void setDir(Dir dir);

protected:
	float baseSpeed;
	float maxSpeed;
	float baseAcceleration;
	float baseDeceleration;

	ScreenImage *graphic;
};