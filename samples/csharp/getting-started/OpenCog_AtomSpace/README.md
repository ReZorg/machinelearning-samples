# OpenCog AtomSpace Sample

| ML.NET version | API type | Status | App Type | Data type | Scenario | ML Task | Algorithms |
|----------------|----------|--------|----------|-----------|----------|---------|------------|
| N/A | Custom Implementation | Up-to-date | Console app | In-memory | Knowledge Representation & Reasoning | Symbolic AI | Hypergraph, Pattern Matching, PLN |

## Overview

This sample demonstrates an implementation of **OpenCog-style** knowledge representation and reasoning in C#. [OpenCog](https://opencog.org/) is an open-source Artificial General Intelligence (AGI) framework that uses a hypergraph-based knowledge store called **AtomSpace**.

While this is not a direct binding to OpenCog (which is written in C++ with Python/Scheme bindings), it implements the core concepts and patterns used in OpenCog for educational purposes and to demonstrate how symbolic AI approaches can be integrated with .NET applications.

## What is OpenCog?

OpenCog is a framework for AGI that consists of several key components:

1. **AtomSpace**: A hypergraph database for knowledge representation
2. **PLN (Probabilistic Logic Networks)**: A reasoning system for uncertain inference
3. **ECAN (Economic Attention Networks)**: An attention allocation system
4. **Pattern Matcher**: A query engine for finding patterns in the knowledge base

## Key Concepts Implemented

### 1. Atoms (Nodes and Links)

In OpenCog, all knowledge is represented as **Atoms**:

- **Nodes**: Represent concepts, entities, or values (e.g., "Dog", "42", "can_fly")
- **Links**: Represent relationships between atoms (e.g., InheritanceLink, EvaluationLink)

```csharp
// Creating concept nodes
var dog = atomSpace.AddNode(NodeTypes.Concept, "Dog");
var animal = atomSpace.AddNode(NodeTypes.Concept, "Animal");

// Creating an inheritance relationship: Dog is-a Animal
atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), dog, animal);
```

### 2. Truth Values

Each atom has a **TruthValue** that represents the probability and confidence of the knowledge:

- **Strength**: The probability or degree of truth (0.0 to 1.0)
- **Confidence**: The certainty about the strength value (0.0 to 1.0)

```csharp
// High confidence that eagles can fly
var canFly = new TruthValue(strength: 1.0, confidence: 0.99);

// Lower confidence that mammals have fur (some exceptions exist)
var hasFur = new TruthValue(strength: 0.85, confidence: 0.8);
```

### 3. Hypergraph Structure

Unlike traditional graphs where edges connect pairs of nodes, in a hypergraph:
- Links can connect any number of atoms
- Links can reference other links
- This allows for complex, nested knowledge structures

```csharp
// EvaluationLink connecting a predicate to its arguments
// (EvaluationLink
//   (PredicateNode "owns")
//   (ListLink
//     (ConceptNode "John")
//     (ConceptNode "Car")))
var owns = atomSpace.AddNode(NodeTypes.Predicate, "owns");
var john = atomSpace.AddNode(NodeTypes.Concept, "John");
var car = atomSpace.AddNode(NodeTypes.Concept, "Car");
var argList = new Link(LinkTypes.List, john, car);
atomSpace.AddLink(LinkTypes.Evaluation, owns, argList);
```

### 4. Pattern Matching

The pattern matcher allows querying the AtomSpace using patterns with variables:

```csharp
var matcher = new PatternMatcher(atomSpace);

// Find all concepts that inherit from "Animal"
var varX = new Node(NodeTypes.Variable, "X");
var animal = atomSpace.GetNode(NodeTypes.Concept, "Animal");
var pattern = new Link(LinkTypes.Inheritance, varX, animal);

var matches = matcher.Match(pattern);
foreach (var match in matches)
{
    Console.WriteLine($"Found: {match.Bindings["X"]}");
}
```

### 5. Inference Engine

The inference engine implements reasoning rules inspired by PLN:

- **Deduction Rule**: If A inherits from B, and B inherits from C, then A inherits from C
- **Inversion Rule**: Generate inverse relationships with adjusted confidence
- **Modus Ponens**: If A implies B, and A is true, then B is true

```csharp
var engine = new InferenceEngine(atomSpace);
var newLinks = engine.RunInferenceToFixpoint();
// Now Dog is known to inherit from Animal (through Mammal)
```

## Sample Output

```
======================================================
OpenCog AtomSpace Sample - Knowledge Representation
======================================================

STEP 1: Building the Knowledge Base
------------------------------------
Creating concept nodes for animal taxonomy...
Creating inheritance links (is-a relationships)...
Creating predicate nodes for properties...
Creating evaluation links (predicate applications)...
Knowledge base built with 20 atoms.

STEP 2: Pattern Matching and Queries
-------------------------------------
Query 1: Find all concepts that inherit from 'Mammal'
  - Dog is a Mammal
  - Cat is a Mammal

Query 2: Find all concepts that 'Dog' inherits from
  - Dog is a Mammal

STEP 3: Inference and Reasoning
--------------------------------
Before inference - Inheritance links:
  - Mammal inherits from Animal <1.00, 0.99>
  - Bird inherits from Animal <1.00, 0.99>
  - Dog inherits from Mammal <1.00, 0.99>
  ...

Running inference engine (Deduction Rule)...
Inference created 4 new links.

After inference - Inheritance links (including inferred):
  - Dog inherits from Animal <1.00, 0.79>
  - Cat inherits from Animal <1.00, 0.79>
  - Eagle inherits from Animal <1.00, 0.79>
  - Penguin inherits from Animal <1.00, 0.79>
  ...
```

## Project Structure

```
OpenCog_AtomSpace/
├── OpenCog_AtomSpace.sln
├── README.md
└── OpenCog/
    └── OpenCogConsoleApp/
        ├── OpenCog_AtomSpace.csproj
        ├── Program.cs
        └── DataStructures/
            ├── Atom.cs           # Base atom class with truth/attention values
            ├── Node.cs           # Node implementation
            ├── Link.cs           # Link implementation
            ├── AtomSpace.cs      # Hypergraph knowledge store
            ├── PatternMatcher.cs # Pattern matching query engine
            └── InferenceEngine.cs # PLN-inspired reasoning engine
```

## Building and Running

```bash
cd samples/csharp/getting-started/OpenCog_AtomSpace
dotnet restore
dotnet build
dotnet run --project OpenCog/OpenCogConsoleApp
```

## Use Cases

This approach to knowledge representation is useful for:

- **Knowledge Graphs**: Building and querying semantic knowledge bases
- **Expert Systems**: Encoding domain knowledge and inference rules
- **Natural Language Understanding**: Representing meaning and relationships
- **Cognitive Architectures**: Building AI systems that reason symbolically
- **Ontology Management**: Defining and working with concept hierarchies

## Further Reading

- [OpenCog Wiki](https://wiki.opencog.org/)
- [AtomSpace Documentation](https://wiki.opencog.org/w/AtomSpace)
- [PLN Book](https://wiki.opencog.org/w/PLN_Book)
- [OpenCog Hyperon](https://hyperon.opencog.org/) - The next generation of OpenCog

## Comparison with ML.NET

While ML.NET focuses on statistical machine learning (classification, regression, etc.), this OpenCog-style implementation demonstrates **symbolic AI** approaches:

| Aspect | ML.NET | OpenCog/AtomSpace |
|--------|--------|-------------------|
| Approach | Statistical/Neural | Symbolic/Logic-based |
| Knowledge | Learned from data | Explicitly encoded |
| Reasoning | Pattern recognition | Logical inference |
| Explainability | Often opaque | Transparent rules |
| Best for | Prediction tasks | Knowledge representation |

In practice, modern AGI systems often combine both approaches (neuro-symbolic AI), using ML.NET for perception and pattern recognition, and symbolic systems like AtomSpace for reasoning and knowledge management.
