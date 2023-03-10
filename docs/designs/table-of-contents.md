# Table of Contents

## Introduction

Table of contents file which is also called TOC file is used to organize file structure and to provide navigation in the published page. TOC Files must have file name toc.md, toc.yml or toc.json, notice that file name is upper case TOC on case sensitive systems.

> [Note]
> Only one or zero file(toc.md, toc.yml or toc.json) is allowed to exist in one folder, otherwise output-path-conflict will be raised during DocFX build.

## Markdown format TOC `toc.md`

`toc.md` leverages Markdown [Atx-style headers](http://daringfireball.net/projects/markdown/syntax#header) which use hash characters at the start of the line to represent the TOC levels. We call each line starting with hash characters a *TOC Item*. A *TOC Item* with higher level is considered as the child of the nearest upper *TOC Item* with less level. A sample `toc.md` is as below:

```md
# [Header1](href)
## [Header1.1](href)
# Header2
## [Header2.1](href)
### [Header2.1.1](href)
## [Header2.2](href)
# @UidForAnArticle
```

For a *TOC Item*, it can be either plain text, or a [Markdown inline link](http://daringfireball.net/projects/markdown/syntax#link), or `@uid` as the shortcut for cross reference.

Three kinds of links are supported:

1. Absolute path, for example, `http://example.net`.
2. Relative path, for example, `../example.md`. This kind of link has several advanced usages and is described in detail [below](#toc-item-link-href).
3. URI with `xref` scheme, for example, `xref:System.String`, the value is the `uid` of the file to be cross referenced.

## YAML format TOC `toc.yml`

It represents a structured data model and conforms to the [YAML standard](http://www.yaml.org/spec/1.2/spec.html)

```yml
- name: Topic1
  href: Topic1.md
- name: Topic2
  href: Topic2.md
  items:
    - name: Topic2_1
      href: Topic2_1.md
```

### Data model for `toc.yml`

The data model for `toc.yml` is either an **array** of *TOC Item Object*, or a dictionary with property **items** being an **array** of *TOC Item Object*.

### *TOC Item Object*

*TOC Item Object* represents the data model for each *TOC Item*.

> [!Note]
> All the property names are **case sensitive**.

#### Properties(input)

Property Name                    | Type                       | Description
-------------------------------- | -------------------------- | ---------------------------
*name*                           | string                     | Specifies the title of the *TOC Item*.
*href*                           | string                     | Specifies the hyperlink of the *TOC Item*. Can be either *toc-include*(see [Link to another *TOC File*](#link-to-another-toc-file), [Link to a folder](#link-to-a-folder)), or simple reference(absolute-link, external-link or relative-link-to-article). When its value is another TOC file, it is a TOC include, and all the items inside the included toc are considered as the child of current *TOC Item*.
*tocHref*<sub>*advanced*</sub>   | string                     | Specifies *toc-include* or absolute reference.
*topicHref*<sub>*advanced*</sub> | string                     | Specifies the topic href of the *TOC Item*. It is useful when *href* is linking to a folder or linking to a toc, and it must **NOT** be *toc-include*.
*uid*                            | string                     | Specifies the `uid` of referenced *TOC Item*. If the value is set, it overwrites the value of *href*, it will also fill the `name` of the *TOC Item* with uid display name if it's not set.
*items*                          | array of *TOC Item Object* | Specifies the children *TOC Items* of current *TOC Item*.


#### Properties(Advanced)
These properties are useful when a TOC links another TOC.  
Possible usage samples:
#   | tocHref         | topicHref          | href           | comment
--- | --------------- | ------------------ | -------------- | -------
1   | folder/         | -                  | article.md     |
2   | -               | article.md         | folder/toc.md  |
3   | folder/         | /absolute/path     | -              | typically used in breadcrumb
(see below [Toc Item Link `href`](#toc-item-link-href) for more detailed explanation)

## Toc Item Link `href`

If a *TOC Item* is linking to some relative path, there are three cases:

1. Linking to another *TOC File*, for example, `href: examples/toc.md`.
2. Linking to a folder, which means, the value of the link ends with `/` explicitly, for example, `href: examples/`
3. Linking to some local file.

Each case is described in detail below.

### Link to another *TOC File*

If the *TOC Item* is linking to some other *TOC File*, it is considered as a placeholder of the referenced *TOC File*, and DocFX will extract content from that *TOC File* and insert into current *TOC Item* **recursively**.

This technique is always used when you want to combine several *TOC File*s into one single *TOC File*.

If `topicHref` is set for this *TOC Item*, it will be considered as the `href` of the expanded *TOC Item*.

For example, one `toc.yml` file is like below:

```yml
- name: How-to tutorials
  href: howto/toc.yml
  topicHref: howto/overview.md
```

It references to the `toc.yml` file under folder `howto`, with the following content:

```yaml
- name: "How-to1"
  href: howto1.md
- name: "How-to2"
  href: howto2.md
```

DocFX processes these `toc.yml` files and expands the below `toc.yml` file into:

```yaml
- name: How-to tutorials
  href: howto/overview.md
  topicHref: howto/overview.md
  items:
    - name: "How-to1"
      href: howto/howto1.md
      topichref: howto/howto1.md
    - name: "How-to2"
      href: howto/howto2.md
      topichref: howto/howto2.md
```

> [!NOTE]
> The referenced `toc.yml` file under `howto` folder will not be built into the output folder.

### Link to a folder

If the *Toc Item* is linking to a folder, ending with `/` explicitly, the link value for the *Toc Item* is determined in the following steps:

1. If `topicHref` or `topicUid` is set, the link value is resolved using `topicUid` then `topicHref`
2. If `topicHref` or `topicUid` is not set, DocFX searches for *Toc File* under the folder **non-recursively**, and get the first [relative link to local file](#link-to-local-file) as the link value for current *Toc Item*. For example, if the *Toc Item* is `href: article/`, and the content of `article/toc.yml` is as follows:

    ```yaml
    toc.yml: |
      - name: parent
        href: article/
    article/toc.yml: |
      - name: Topic1
        href: topic1.md
    ```
    The link value for the *Toc Item* is resolved to `article/topic1.md`.
    ```yaml
    toc.yml: |
      - name: parent
        href: article/topic.md
    ```
3. If there is no *Toc File* under the folder, the link value keeps unchanged.

## Local file referenced by TOC

If a *Toc Item* is linking to a local file, we call this local file *In-Toc File*. The link can also contains query string and fragment like `href: examples/overview.md?branch=master#row=4`. It's represented by `_tocRel` in the local file's `metadata`.

### Not-In-Toc Files

When a local file is not referenced by any *Toc Item*, we call this local file *Not-In-Toc File*(orphan file). Its *TOC File* is the nearest *TOC File* in input folder from the same folder as the local file to the root folder.

### Nearest toc

**Near** means the relative path to toc file has less subdirectory count, or less parent directory count when the subdirectory counts are the same, e.g. the relative path to toc '../../a/toc.md' is nearer than 'b/c/toc.md'.