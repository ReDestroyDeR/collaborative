# ИС «Учёт студентов» — учебный проект (коллективная разработка)

Репозиторий создан для выполнения лабораторных работ по дисциплине
«Разработка кода информационных систем» (коллективная разработка).

Предметная область — учёт студентов вуза: факультеты, кафедры, группы,
студенты, преподаватели, дисциплины, учебный план, расписание занятий,
успеваемость и посещаемость.

## Структура решения (.NET 8, многослойная архитектура)

```
StudentAccounting.sln
├── src/
│   ├── StudentAccounting.Dal/   # слой доступа к данным (DAL): сущности, IRepository, хранилища
│   ├── StudentAccounting.Bll/   # слой бизнес-логики (BLL): сервисы, модуль стипендий
│   ├── StudentAccounting.Ui/    # слой пользовательского интерфейса (UI): консольное приложение
│   └── InterfacesDemo/          # ЛР2: демонстрация интерфейсов C# (4 задания)
├── tests/
│   └── StudentAccounting.Tests/ # ЛР4: модульные тесты xUnit + Moq, 100 % покрытие ветвей
├── diagrams/                    # диаграммы: ER, UML, IDEF0, компоненты (PlantUML/matplotlib)
└── .github/workflows/ci.yml     # CI GitHub Actions: build + test + coverage
```

## Сборка и запуск

```bash
dotnet build
dotnet run --project src/StudentAccounting.Ui
dotnet run --project src/InterfacesDemo
dotnet test                                   # 100 тестов
dotnet test --collect:"XPlat Code Coverage"   # измерение покрытия (100 % строк и ветвей)
```

## Инструменты

- Язык: C# 12, платформа .NET 8 (LTS)
- IDE: Visual Studio 2022 / JetBrains Rider
- Диаграммы: PlantUML
- Система контроля версий: Git (GitHub)
- CI: GitHub Actions (`.github/workflows/ci.yml`)

## Непрерывная интеграция

`.github/workflows/ci.yml`: restore → build (Release) → test → покрытие кода.
Запускается при каждом push в main/develop/feature-ветки и при каждом pull request.
Рекомендуемая защита ветки main: Settings → Branches → Require a pull request before
merging + Require status checks (CI / build-and-test).
