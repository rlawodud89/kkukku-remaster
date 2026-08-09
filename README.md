# 🐇 꾸꾸의 이불 가게

잠을 잃은 마을에서 달빛토끼 꾸꾸가 이불을 제작하고 판매하며 포근 에너지를 모아 마을의 밤을 되찾는 **싱글플레이** 감성 운영 게임입니다.

## 🎮 실행 방법

### 빌드 다운로드

[Windows 빌드 다운로드](https://drive.google.com/drive/folders/1__o8amQpZdAhaWlcKIoTIX6gcfhoHGO3?usp=drive_link)

### 필수 환경

- Windows 기반 PC
- 별도의 Unity 설치 없이 빌드 파일로 실행 가능

## ✨ 주요 기능

| 기능 | 설명 |
| --- | --- |
| 이불 제작 | 개인 제작대에서 재료를 조합해 레시피를 획득하고, 직원에게 제작을 지시해 완성된 이불을 재고함에 저장합니다. |
| 이불 판매 | 이불장에 상품을 배치하고 영업을 시작하면 손님이 방문해 상품을 구매합니다. 판매 즉시 재화와 포근 에너지를 획득합니다. |
| 채집 | 랜덤 위치에 등장하는 간식을 일정 횟수 클릭해 획득합니다. 제한 시간이 지나면 위치와 클릭 횟수가 초기화됩니다. |
| 낚시 | 하단 바와 내려오는 재료의 위치를 맞춰 클릭하는 리듬 게임 방식으로 재료를 획득합니다. 위치가 가까울수록 보너스 획득 확률이 높아집니다. |
| 상점 | 신성 재료, 직원, 인테리어, 도구 등을 구매하고 수량을 조절합니다. 재화나 재고 슬롯이 부족하면 안내 메시지를 출력합니다. |
| 튜토리얼 | 프롤로그 이후 단계별 안내와 하이라이트를 제공하고, 각 단계의 목표를 완료하면 다음 단계로 진행합니다. |
| 엔딩 | 레벨 10 달성 후 뒤척임 안개숲이 해금되며, 첫 입장 시 엔딩 애니메이션을 재생합니다. 이후에는 엔딩 이후의 공간을 확인할 수 있습니다. |

**현재 상태: 개발 완료**

## 🏗️ 게임 아키텍처

### SQLite + ScriptableObject 하이브리드 데이터 관리

런타임 상태 데이터와 고정 게임 데이터를 분리해 관리합니다.

- **SQLite**: 재화, 보유 이불, 퀘스트 진행도 등 런타임 중 변경되는 데이터
- **ScriptableObject**: 아이템 이름, 판매 가격 등 런타임 중 변경되지 않는 게임 데이터

관련 코드:
- [Data Entities](https://github.com/rlawodud89/kkukku-remaster/tree/main/Assets/Scripts/Core/DataEntites)
- [ScriptableObject 데이터](https://github.com/rlawodud89/kkukku-remaster/tree/main/Assets/ScriptableObjects)

### 메모리 캐시 중심 구조

게임 시작 시 SQLite 데이터를 한 번에 로드해 `GameData`와 `Aggregate`로 구성합니다. 이후 런타임에서는 메모리 데이터를 사용해 씬 전환마다 DB를 조회하지 않습니다.

```text
Game Start
    ↓
SaveSystem
    ↓
SaveRepository.LoadAll()
    ↓
Aggregate 생성
    ↓
GameData
    ↓
ServiceLocator
```

### 중앙 저장 관리자

각 시스템은 DB에 직접 접근하지 않고 `GameData`의 `Aggregate`를 통해 데이터를 변경합니다.

```text
UI / Controller / Gameplay Script
            ↓
      ServiceLocator
            ↓
         GameData
            ↓
 Aggregate (User / Inventory / Quest ...)
```

`Aggregate`는 데이터 변경 시 Dirty 상태를 기록하고, `DirtyDataRegistry`는 변경된 Aggregate를 관리합니다.

관련 코드:
- [GameData](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/DataSystems/GameData.cs)
- [Aggregate](https://github.com/rlawodud89/kkukku-remaster/tree/main/Assets/Scripts/Core/DataSystems/Aggregates)
- [DirtyDataRegistry](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/DataSystems/DirtyDataRegistry.cs)
- [ServiceLocator](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/DataSystems/ServiceLocator.cs)

### 자동 저장

`SaveSystem`이 저장 주기를 관리하고, 저장 시점에 `SaveService`가 Dirty 데이터를 `SaveRepository`로 전달합니다. `SaveRepository`는 `SavePayload`를 해석해 SQLite에 변경 사항을 반영합니다.

```text
Unity Update
    ↓
SaveSystem
    ↓
SaveService.Flush()
    ↓
DirtyDataRegistry
    ↓
SaveRepository
    ↓
SQLite INSERT / UPDATE / DELETE
```

저장 흐름은 변경된 데이터만 처리하며, 저장 완료 후 Dirty 상태를 초기화합니다.

관련 코드:
- [SaveSystem](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/Managers/SaveSystem.cs)
- [SaveService](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/DataSystems/SaveService.cs)
- [SaveRepository](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/DataSystems/SaveRepository.cs)
- [SavePayload](https://github.com/rlawodud89/kkukku-remaster/blob/main/Assets/Scripts/Core/DataSystems/SavePayload.cs)

### 설계 목표

- 씬 로드마다 발생하는 DB 조회 최소화
- 런타임 메모리 데이터 중심 처리
- 변경 사항만 Dirty로 추적
- 일정 주기 자동 저장
- Singleton 대신 Service 기반 구조로 Unity 수명주기에서 저장 시스템 분리
