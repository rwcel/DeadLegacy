using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPoolingEvent : AIEvent
{
	// Event함수 대신 실행 : 죽은 ai들 풀링
	public override void Play()
	{
		base.Play();

		AIPoolingManager.instance.DeathAISetFalse();
	}
}
