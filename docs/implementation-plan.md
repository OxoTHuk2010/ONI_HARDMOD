# План реализации

Этот документ является рабочим планом после стабилизации версии 0.4.7.

## Источник истины

Использовать документы в таком порядке:

1. `todo.txt` - основная спецификация проекта.
2. `docs/implementation-plan.md` - текущий пошаговый план реализации.
3. `docs/game-api-research.md` - подтверждённые ONI API hooks и runtime-находки.
4. `docs/implementation-note-*.md` - фактически реализованное поведение и известные ограничения конкретных версий.

Если документы противоречат друг другу, сначала обновлять `todo.txt`, затем этот план.

## Закрытые решения

0.4 считается закрытым после проверки текущей 0.4.7 в игре.

Не реализуем и не держим в backlog:

* 0.5 Superconductive Technology;
* новые обычные/high-watt провода;
* wire resistance;
* length-based electrical losses;
* voltage drop;
* consumer brownout/degraded production;
* transformer efficiency;
* 0.6 World Temperature.

Причины:

* новые провода и сверхпроводник теряют смысл без механики сопротивления/потерь;
* электрическая модель ONI не даёт достаточно прозрачного направленного пути энергии для аккуратной физической сети без собственной сети поверх игры;
* температурные пресеты мира уже закрываются специализированными модами;
* дублирование этих направлений увеличит риск конфликтов и поддержки без достаточной ценности.

## Следующий активный релиз: 0.7 Asteroid Sizes Experimental

Цель: добавить экспериментальные уменьшенные варианты астероидов для новых миров, не меняя vanilla generation при выключенном модуле.

Целевые режимы:

```text
Vanilla
Half
Quarter
```

`Half` - основной режим 0.7.

`Quarter` - experimental режим, который может быть отключён для отдельных clusters/DLC, если обязательные структуры не помещаются стабильно.

## Фаза A - Worldgen Audit

Роль: Architect / Research.

Задачи:

1. Найти игровые YAML/data файлы cluster/world generation.
2. Определить, как задаются размеры мира, border, стартовая область и biome placement.
3. Найти способ добавлять новые presets без перезаписи vanilla данных.
4. Проверить различия base game и Spaced Out.
5. Найти обязательные структуры:
   * стартовая область;
   * geysers;
   * teleporters;
   * Temporal Tear;
   * POI;
   * story-critical structures;
   * DLC-specific structures.
6. Определить, где можно валидировать worldgen data до старта генерации.
7. Зафиксировать hook points и риски.

Артефакт:

```text
docs/v0.7-worldgen-asteroid-size-audit.md
```

Переход дальше разрешён только если понятно, можно ли сделать data-only прототип.

## Фаза B - Data-Only Prototype

Роль: Senior Software Engineer.

Задачи:

1. Добавить минимальный `WorldGeneration` модуль или подготовить data registration path, если кодовый модуль пока не нужен.
2. Создать первый Half preset через YAML/data overrides.
3. Не менять vanilla presets in-place.
4. Не менять существующие сохранения.
5. Не добавлять runtime-сжатие мира.
6. Добавить config flag:

```json
{
  "World": {
    "Enabled": true,
    "AsteroidSize": "Half"
  }
}
```

7. Убедиться, что `Off` и `Vanilla` полностью сохраняют vanilla generation.

Стоп-условие:

Если data-only path невозможен, зафиксировать почему и перейти к отдельному C# prototype design, не внедряя большой Harmony patch сразу.

## Фаза C - Generation Validator

Роль: Reliability & Safety.

Задачи:

1. Проверять bounds до генерации или сразу после сборки worldgen data.
2. Валидировать:
   * стартовый биом;
   * safe spawn area;
   * world border;
   * biome template bounds;
   * обязательные structures;
   * geysers/POI density;
   * paired teleporters;
   * DLC-only content guards.
3. При ошибке:
   * не начинать генерацию, если ошибка обнаружена до неё;
   * показывать понятное предупреждение;
   * писать diagnostics;
   * не менять vanilla presets.

Артефакт:

```text
docs/implementation-note-0.7.0.md
```

## Фаза D - Seed Test Harness

Роль: Test Engineer.

Задачи:

1. Подготовить ручной или полуавтоматический seed-test журнал.
2. Для каждого режима прогнать минимум 20-50 seed на этапе прототипа.
3. Проверить:
   * base game;
   * Spaced Out;
   * разные стартовые астероиды;
   * установленные DLC;
   * отсутствие worldgen exception;
   * наличие обязательных объектов;
   * создание колонии;
   * save/load.

Формат журнала:

```text
seed
world preset
asteroid size mode
DLC
result
generation time
exception
missing entities
validation errors
```

## Фаза E - Quarter Experimental

Роль: Architect / Safety.

Задачи:

1. Реализовать Quarter только после рабочего Half.
2. Проверить, какие clusters могут физически вместить обязательные structures.
3. Для несовместимых clusters:
   * отключать режим;
   * либо показывать explicit experimental warning;
   * не пытаться автоматически ломать обязательные constraints.

Критерии:

* Quarter не должен ломать vanilla и Half.
* Quarter может быть помечен experimental даже в релизе 0.7.

## Фаза F - Documentation And Release

Роль: Documentation / Release.

Задачи:

1. Обновить README:
   * что делает 0.7;
   * что не делает 0.7;
   * как включить Half/Quarter;
   * какие риски у experimental mode.
2. Обновить changelog.
3. Обновить `docs/game-api-research.md`.
4. Собрать Release.
5. Запустить unit tests.
6. Установить локальный мод.
7. Подготовить ручной чеклист проверки в ONI.

## Definition Of Done 0.7

0.7 готова, если:

1. Vanilla generation не меняется при выключенном модуле.
2. Half mode стабильно создаёт миры на выбранном наборе clusters/seeds.
3. Quarter mode либо стабилен, либо явно помечен experimental и ограничен по clusters.
4. Нет runtime-сжатия существующих миров.
5. Нет перезаписи vanilla worldgen файлов.
6. Есть diagnostics для неудачной генерации.
7. Документированы ограничения для base game, Spaced Out и DLC.
8. Есть понятный rollback: выключение модуля возвращает vanilla worldgen для новых миров.

## Следующие направления после 0.7

После 0.7 остаются только направления, которые всё ещё имеют самостоятельную ценность:

* Radiative Heat Experimental;
* Fluid Pressure Experimental;
* release hardening / Workshop packaging.
