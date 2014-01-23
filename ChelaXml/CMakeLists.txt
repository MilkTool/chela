set(Xml_SRC
    Formatting.chela
    NameTable.chela
    ReadState.chela
    WhitespaceHandling.chela
    WriteState.chela
    XmlException.chela
    XmlNamespaceManager.chela
    XmlNameTable.chela
    XmlNodeType.chela
    XmlParserContext.chela
    XmlReader.chela
    XmlResolver.chela
    XmlSpace.chela
    XmlTextReader.chela
    XmlUrlResolver.chela
    XmlWriter.chela
)

chela_module(ChelaXml LIBRARY SRC ${Xml_SRC})

