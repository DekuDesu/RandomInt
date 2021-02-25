# RandomInt
### An out of the box Random Integer Generator

## Features
- Simple usage
- RANDOM.ORG integer generation
- ```System.Random``` backup

## Usage

### Installation
Import the file using the 'Add Existing' option in visual studio and select Generator.cs.

### Initializing
Before you can use the generator to get random integer or arrays of integers, you must initialize the ```HttpClient``` that the generator uses. To do this run 
```csharp
RandomInt.Generator.InitializeClient();
```

### Getting a Single Integer
After you initialize the client to get a single integer use:
```csharp
int randomNumber = Generator.Next();
```

#### `Generator.Next` Parameters
The ```Generator.Next(int min = -1000000000, int 1000000000)``` has the following parameters:
| Parameter | Type | Range | Description |
|--|--|--|--|
| `min` | `Int32` | `-1e9 - 1e9` |  The smallest number you want to be returned (Inclusive) |
| `max` | `Int32` | `-1e9 - 1e9` |  The largest number you want to be returned (Inclusive) |

#### Examples
Example:
```csharp
int number = Generator.Next(1,1);
Console.WriteLine($"Your luck number is: {number}");
```
Output:
```
Your luck number is: 1
```
Example:
```csharp
int number = Generator.Next(int.MinValue,int.MaxValue);
Console.WriteLine($"Your luck number is: {number}");
```
Output:
```
Your luck number is: 1000000000
```

### Getting a random array of integers
To get an array of integers you can use the following method:
```
int[] randomNumbers = Generator.NextSet();
```

#### `Generator.Next` Parameters
The ```Generator.Next(int min = -1000000000, int 1000000000)``` has the following parameters:
| Parameter | Type | Range | Description |
|--|--|--|--|
| `min` | `Int32` | `-1e9 - 1e9` |  The smallest number you want to be returned (Inclusive) |
| `max` | `Int32` | `-1e9 - 1e9` |  The largest number you want to be returned (Inclusive) |
| `n` | `Int32` | `1 - 1e4` |  The amount of integers you want in the set(the length of the array) |
| `numberBase` | `Int32` | 2 or 8 or 10 or 16 | The base of the number you want to be returned |
