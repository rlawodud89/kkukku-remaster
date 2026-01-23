using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 설명 : MonoBehaviour
{
    /*
1. 데이터와 관리 (영혼과 뇌)

CustomerData (데이터 클래스): NPC의 상태(쇼핑 중, 결제 중 등)와 어떤 아이템을 골랐는지 정보를 담는 **'신분증'**입니다. 씬이 바뀌어 NPC 오브젝트가 삭제되어도 이 데이터는 남습니다.

ShopManager (싱글톤 & DontDestroyOnLoad): 게임 전체의 **'뇌'**입니다.

연속성: DontDestroyOnLoad 덕분에 씬을 이동해도 사라지지 않고 데이터를 유지합니다.

오프라인 시뮬레이션: SimulateOfflineProgress()는 유저가 다른 씬에 있던 시간을 계산하여 "아, 이 손님은 이미 결제하고 나갔겠구나"라고 판단해 돈을 벌어다 줍니다.

ItemData (ScriptableObject): 이불의 이름, 가격, 이미지를 저장하는 **'도감'**입니다.

2. 생성과 연결 (다리 역할)
데이터를 기반으로 실제 눈에 보이는 캐릭터를 만들어내는 과정입니다.

NPCSpawner (생성기): * 가게 씬이 로드될 때 ShopManager의 리스트를 뒤져서 "아직 가게에 남아있어야 할 손님"들을 찾아냅니다.

그 데이터(CustomerData)를 가진 실제 NPC 프리팹을 화면에 소환합니다.

3. 지능과 길찾기 (길잡이)
NPC가 장애물을 피해 똑똑하게 움직이게 하는 수학적 장치입니다.

Node (지도 조각): 타일맵의 한 칸 한 칸을 데이터화한 것입니다. "여기는 이동 가능한가?(walkable)", "목표까지 얼마나 먼가?(gCost, hCost)"를 계산하기 위한 단위입니다.

Pathfinding (길찾기 알고리즘): (NPC가 참조하는 스크립트) Node들을 계산하여 출발지부터 목적지까지 **최단 경로(타일 리스트)**를 뽑아줍니다.

4. 실제 행동과 시각화 (배우)
우리가 화면에서 보는 NPC의 움직임과 겉모습을 담당합니다.

NPCAI (행동 제어):

상태 머신 (BehaviorRoutine): 코루틴을 사용해 "이동 -> 대기 -> 결제 -> 퇴장"의 순서를 시간 흐름에 따라 실행합니다.

A 이동 (MoveWithAStar):* 길찾기 알고리즘이 준 타일 경로를 하나씩 밟으며 부드럽게 이동합니다.

Y축 정렬 (Update): sortingOrder = y * -100 공식을 통해 NPC가 가구 뒤로 가면 가려지고, 앞으로 오면 가구 위로 보이게 만듭니다.

전체 흐름 요약
가게 오픈: 유저가 버튼을 누르면 ShopManager가 CustomerData를 생성합니다.

소환: NPCSpawner가 그 데이터를 받아 NPCAI 오브젝트를 씬에 만듭니다.

행동: NPCAI는 Pathfinding에 길을 물어봐서 타일맵 위를 걷고, 이불을 고릅니다.

씬 이동: 유저가 다른 씬으로 가면 NPC 오브젝트는 삭제되지만, CustomerData는 ShopManager 안에 안전하게 보관됩니다.

복귀: 다시 가게로 오면 흐른 시간을 계산해 매출을 정산하고, 아직 쇼핑 중인 손님만 다시 소환합니다.
     * 
     * 
     * 
     * 연결
     * 스크립트,변수 이름,연결할 대상
ShopManager,Item Database,생성해둔 ItemData 파일들을 리스트에 넣어야 함
NPCAI (Prefab),Item Display SR,NPC 자식 오브젝트로 만든 SpriteRenderer
NPCAI (Prefab),Walk Tilemap,씬의 Grid 아래에 있는 바닥 타일맵
Pathfinding,Obstacle Layer,가구 오브젝트들이 설정된 Layer (예: Obstacle)
NPCSpawner,Entrance Transform,입구로 지정할 빈 오브젝트
     * 
     */
}
